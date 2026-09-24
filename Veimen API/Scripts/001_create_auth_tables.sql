CREATE TABLE IF NOT EXISTS `user` (
    `user_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `username` VARCHAR(50) NOT NULL,
    `email` VARCHAR(150) NOT NULL,
    `password_hash` VARCHAR(100) NOT NULL,
    `full_name` VARCHAR(150) NULL,
    `active` TINYINT(1) NOT NULL DEFAULT 1,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `last_login_at` DATETIME NULL,
    PRIMARY KEY (`user_id`),
    UNIQUE KEY `uq_user_username` (`username`),
    UNIQUE KEY `uq_user_email` (`email`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

CREATE TABLE IF NOT EXISTS `refresh_token` (
    `refresh_token_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `user_id` BIGINT UNSIGNED NOT NULL,
    `token_sha256` CHAR(64) NOT NULL,
    `expires_at` DATETIME NOT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `revoked_at` DATETIME NULL,
    `replaced_by_token` CHAR(64) NULL,
    `ip_address` VARCHAR(45) NULL,
    `user_agent` VARCHAR(255) NULL,
    PRIMARY KEY (`refresh_token_id`),
    UNIQUE KEY `uq_refresh_token_token` (`token_sha256`),
    KEY `idx_refresh_token_user` (`user_id`),
    CONSTRAINT `fk_refresh_token_user`
        FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;