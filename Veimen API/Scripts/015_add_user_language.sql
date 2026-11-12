-- Idioma preferido del usuario (user.language): "Español" (default) o "Inglés".
-- La columna puede existir ya en algunas bases (creada a mano); el script es
-- idempotente y en ese caso no hace nada.

SET @has_column := (
  SELECT COUNT(*)
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'user'
    AND COLUMN_NAME = 'language'
);

SET @sql := IF(
  @has_column = 0,
  'ALTER TABLE `user` ADD COLUMN `language` VARCHAR(100) NULL DEFAULT ''Español'' AFTER `client_id`',
  'DO 0'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
