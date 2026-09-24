-- Sistema de perfiles y permisos (RBAC simple: user -> profile -> profile_permission -> permission).
-- Los códigos de permiso deben coincidir con Services/Permissions.cs (policies y claims 'perm').

CREATE TABLE IF NOT EXISTS `profile` (
    `profile_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `name` VARCHAR(50) NOT NULL,
    `description` VARCHAR(200) NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`profile_id`),
    UNIQUE KEY `uq_profile_name` (`name`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

CREATE TABLE IF NOT EXISTS `permission` (
    `permission_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `code` VARCHAR(60) NOT NULL,
    `description` VARCHAR(200) NULL,
    PRIMARY KEY (`permission_id`),
    UNIQUE KEY `uq_permission_code` (`code`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

CREATE TABLE IF NOT EXISTS `profile_permission` (
    `profile_id` BIGINT UNSIGNED NOT NULL,
    `permission_id` BIGINT UNSIGNED NOT NULL,
    PRIMARY KEY (`profile_id`, `permission_id`),
    CONSTRAINT `fk_profile_permission_profile`
        FOREIGN KEY (`profile_id`) REFERENCES `profile` (`profile_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_profile_permission_permission`
        FOREIGN KEY (`permission_id`) REFERENCES `permission` (`permission_id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

ALTER TABLE `user`
    ADD COLUMN `profile_id` BIGINT UNSIGNED NULL,
    ADD CONSTRAINT `fk_user_profile`
        FOREIGN KEY (`profile_id`) REFERENCES `profile` (`profile_id`);

-- Datos iniciales: permisos conocidos por el backend (ver Services/Permissions.cs).
INSERT INTO `permission` (`code`, `description`) VALUES
    ('prompts.read', 'Consultar prompts'),
    ('prompts.write', 'Crear, editar y eliminar prompts'),
    ('service-requests.read', 'Consultar service requests (lista, dashboard y trazas)');

-- Perfil de administrador con todos los permisos y uno de solo lectura como ejemplo.
INSERT INTO `profile` (`name`, `description`) VALUES
    ('Admin', 'Acceso total a todas las opciones'),
    ('Reader', 'Solo lectura');

INSERT INTO `profile_permission` (`profile_id`, `permission_id`)
SELECT p.profile_id, perm.permission_id
FROM `profile` p
CROSS JOIN `permission` perm
WHERE p.name = 'Admin';

INSERT INTO `profile_permission` (`profile_id`, `permission_id`)
SELECT p.profile_id, perm.permission_id
FROM `profile` p
INNER JOIN `permission` perm ON perm.code IN ('prompts.read', 'service-requests.read')
WHERE p.name = 'Reader';

-- Migración inicial: asignar todos los usuarios existentes al perfil Admin.
-- Ajustar según corresponda (o asignar Reader manualmente a quien no deba administrar).
UPDATE `user`
SET `profile_id` = (SELECT `profile_id` FROM `profile` WHERE `name` = 'Admin')
WHERE `profile_id` IS NULL;
