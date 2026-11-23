-- Clave primaria autonumérica `id` en email_evaluation_dataset. `case` deja de ser PK:
-- pasa a ser un campo de texto editable (con índice para búsquedas).
-- Debe ejecutarse después de 017_email_evaluation_case_column.sql.
-- Idempotente: si la columna `id` ya existe, no hace nada.

SET @has_id := (
  SELECT COUNT(*)
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'email_evaluation_dataset'
    AND COLUMN_NAME = 'id'
);

SET @sql := IF(
  @has_id = 0,
  'ALTER TABLE `email_evaluation_dataset`
     DROP PRIMARY KEY,
     ADD COLUMN `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT FIRST,
     ADD PRIMARY KEY (`id`),
     ADD INDEX `idx_email_eval_case` (`case`)',
  'DO 0'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
