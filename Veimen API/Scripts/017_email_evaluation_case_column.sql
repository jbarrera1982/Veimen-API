-- La PK de email_evaluation_dataset pasa de `case_id` (VARCHAR(50), solo símbolos
-- permitidos por la API) a `case` (VARCHAR(255), texto libre sin restricción de caracteres).
-- `case` es palabra reservada en MySQL: se cita siempre con backticks.
-- Idempotente: si la columna ya se llama `case`, no hace nada.

SET @has_old := (
  SELECT COUNT(*)
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'email_evaluation_dataset'
    AND COLUMN_NAME = 'case_id'
);

SET @sql := IF(
  @has_old = 1,
  'ALTER TABLE `email_evaluation_dataset` CHANGE COLUMN `case_id` `case` VARCHAR(255) NOT NULL',
  'DO 0'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
