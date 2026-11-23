-- Correcciones de esquema de email_evaluation_dataset tras la revisión del diseño:
--  1) created_at / updated_at pasan a NOT NULL con DEFAULT CURRENT_TIMESTAMP (igual que el
--     resto de las tablas; la API ya los completa, esto protege inserciones directas).
--  2) Índice del listado (client_id, deleted, created_at): cubre el filtro por cliente y el
--     ORDER BY created_at DESC, id DESC de GET /api/email-evaluation-dataset.
--  3) Se eliminan índices sin uso útil:
--     - idx_email_eval_client_deleted: queda cubierto por el índice del listado (mismo prefijo).
--     - idx_email_eval_updated: ninguna consulta ordena o filtra por updated_at.
--     - idx_email_eval_case: un LIKE '%texto%' no puede usar un índice B-tree.
-- El índice del listado se crea ANTES de borrar idx_email_eval_client_deleted, porque la FK
-- fk_email_eval_client necesita siempre un índice que empiece por client_id.
-- Idempotente: se puede ejecutar más de una vez sin error.

-- 2) Índice del listado.
SET @has_listing := (
  SELECT COUNT(*)
  FROM information_schema.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'email_evaluation_dataset'
    AND INDEX_NAME = 'idx_email_eval_listing'
);
SET @sql := IF(
  @has_listing = 0,
  'ALTER TABLE `email_evaluation_dataset` ADD INDEX `idx_email_eval_listing` (`client_id`, `deleted`, `created_at`)',
  'DO 0'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 3) Índices redundantes o sin uso.
SET @has_idx := (
  SELECT COUNT(*)
  FROM information_schema.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'email_evaluation_dataset'
    AND INDEX_NAME = 'idx_email_eval_client_deleted'
);
SET @sql := IF(
  @has_idx > 0,
  'ALTER TABLE `email_evaluation_dataset` DROP INDEX `idx_email_eval_client_deleted`',
  'DO 0'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @has_idx := (
  SELECT COUNT(*)
  FROM information_schema.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'email_evaluation_dataset'
    AND INDEX_NAME = 'idx_email_eval_updated'
);
SET @sql := IF(
  @has_idx > 0,
  'ALTER TABLE `email_evaluation_dataset` DROP INDEX `idx_email_eval_updated`',
  'DO 0'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @has_idx := (
  SELECT COUNT(*)
  FROM information_schema.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'email_evaluation_dataset'
    AND INDEX_NAME = 'idx_email_eval_case'
);
SET @sql := IF(
  @has_idx > 0,
  'ALTER TABLE `email_evaluation_dataset` DROP INDEX `idx_email_eval_case`',
  'DO 0'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 1) Fechas obligatorias. Las filas con NULL (si las hubiera) reciben la hora actual.
UPDATE `email_evaluation_dataset` SET `created_at` = CURRENT_TIMESTAMP WHERE `created_at` IS NULL;
UPDATE `email_evaluation_dataset` SET `updated_at` = CURRENT_TIMESTAMP WHERE `updated_at` IS NULL;

ALTER TABLE `email_evaluation_dataset`
  MODIFY COLUMN `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  MODIFY COLUMN `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
