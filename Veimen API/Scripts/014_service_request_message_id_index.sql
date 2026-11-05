-- Acelera el LEFT JOIN del badge de Procesamiento por service_request.message_id.
-- Idempotente: no falla si el índice ya existe.

SET @has_index := (
  SELECT COUNT(*)
  FROM information_schema.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'service_request'
    AND INDEX_NAME = 'idx_service_request_message_id'
);

SET @sql := IF(
  @has_index = 0,
  'ALTER TABLE service_request ADD INDEX idx_service_request_message_id (message_id)',
  'DO 0'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
