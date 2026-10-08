-- Borrado lógico para `client` y `prompt`: las filas no se eliminan, se marcan con
-- `deleted = 1` y los GET filtran `deleted = 0` (ver ClientRepository/PromptRepository).
-- MySQL 8 no soporta ADD COLUMN IF NOT EXISTS: aplicar este script una sola vez.

ALTER TABLE `client`
    ADD COLUMN `deleted` TINYINT(1) NOT NULL DEFAULT 0;

ALTER TABLE `prompt`
    ADD COLUMN `deleted` TINYINT(1) NOT NULL DEFAULT 0;
