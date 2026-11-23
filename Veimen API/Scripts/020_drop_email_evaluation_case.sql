-- Elimina el campo `case` de email_evaluation_dataset (se retira del modelo y de la API).
-- ATENCIÓN: borra los valores que tuviera la columna. Al eliminar la columna, MySQL quita
-- también los índices que la usan (idx_email_eval_case, si aún existiera).
-- Debe ejecutarse después de 019_email_evaluation_schema_fixes.sql.
-- Idempotente: si la columna ya no existe, no hace nada.

SET @has_case := (
  SELECT COUNT(*)
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'email_evaluation_dataset'
    AND COLUMN_NAME = 'case'
);

SET @sql := IF(
  @has_case = 1,
  'ALTER TABLE `email_evaluation_dataset` DROP COLUMN `case`',
  'DO 0'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
