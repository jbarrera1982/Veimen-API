-- Atribución de los correos escaneados a un cliente (mail_message.client_id).
-- MailRepository.InsertMessageAsync ya inserta client_id y MailService lo toma de la
-- configuración 'Imap:ClientId' (default 1, el valor histórico), así que en bases ya
-- existentes la columna puede estar creada a mano: el script es idempotente.

SET @has_column := (
  SELECT COUNT(*)
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'mail_message'
    AND COLUMN_NAME = 'client_id'
);

SET @sql := IF(
  @has_column = 0,
  'ALTER TABLE mail_message ADD COLUMN client_id BIGINT NOT NULL DEFAULT 1 AFTER id',
  'DO 0'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @has_index := (
  SELECT COUNT(*)
  FROM information_schema.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'mail_message'
    AND INDEX_NAME = 'idx_mail_message_client_date'
);

SET @sql := IF(
  @has_index = 0,
  'ALTER TABLE mail_message ADD INDEX idx_mail_message_client_date (client_id, message_date)',
  'DO 0'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
