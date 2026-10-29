CREATE TABLE mail_message (
  id BIGINT AUTO_INCREMENT PRIMARY KEY,
  mailbox VARCHAR(50) NOT NULL DEFAULT 'INBOX',
  uid BIGINT UNSIGNED NOT NULL,
  message_id VARCHAR(512) NULL,
  from_address VARCHAR(512) NULL,
  subject VARCHAR(1024) NULL,
  message_date DATETIME NOT NULL,
  received_at DATETIME NOT NULL,
  UNIQUE KEY uk_mail_message_mailbox_uid (mailbox, uid),
  INDEX idx_mail_message_message_date (message_date),
  INDEX idx_mail_message_mailbox_date (mailbox, message_date)
);

CREATE TABLE mail_daily_summary (
  summary_id BIGINT AUTO_INCREMENT PRIMARY KEY,
  mailbox VARCHAR(50) NOT NULL DEFAULT 'INBOX',
  summary_date DATE NOT NULL,
  count INT NOT NULL DEFAULT 0,
  last_recalculated_at DATETIME NOT NULL,
  UNIQUE KEY uk_mail_daily_summary_mailbox_date (mailbox, summary_date),
  INDEX idx_mail_daily_summary_date (summary_date)
);

CREATE TABLE mail_scan_state (
  id INT AUTO_INCREMENT PRIMARY KEY,
  mailbox VARCHAR(50) NOT NULL DEFAULT 'INBOX',
  last_scanned_at DATETIME NOT NULL,
  UNIQUE KEY uk_mail_scan_state_mailbox (mailbox)
);
