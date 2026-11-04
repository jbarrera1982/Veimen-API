-- Teléfono de contacto del usuario (user.phone).
-- Opcional: las filas existentes quedan en NULL. Script idempotente para bases
-- donde la columna ya se haya creado a mano.

SET @has_column := (
  SELECT COUNT(*)
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'user'
    AND COLUMN_NAME = 'phone'
);

SET @sql := IF(
  @has_column = 0,
  'ALTER TABLE `user` ADD COLUMN `phone` VARCHAR(30) NULL AFTER `email`',
  'DO 0'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
