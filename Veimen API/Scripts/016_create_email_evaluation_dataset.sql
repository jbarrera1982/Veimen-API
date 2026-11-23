-- Dataset de correos para evaluar el agente de email.
-- Cada fila es un caso de prueba: `scenario` es el escenario esperado y `content`
-- guarda el HTML original del correo. `from` es palabra reservada en MySQL: se cita.
-- Idempotente: se puede ejecutar más de una vez sin error.

CREATE TABLE IF NOT EXISTS `email_evaluation_dataset` (
    `case_id`    VARCHAR(50)  NOT NULL,
    `scenario`   VARCHAR(150) NOT NULL,
    `from`       VARCHAR(320) NOT NULL,
    `subject`    VARCHAR(500) NULL,
    `content`    LONGTEXT     NULL,
    `client_id`  INT          NOT NULL,
    `deleted`    TINYINT(1)   NOT NULL DEFAULT 0,
    `created_at` DATETIME     NULL,
    `updated_at` DATETIME     NULL,
    `created_by` VARCHAR(100) NULL,
    `updated_by` VARCHAR(100) NULL,
    PRIMARY KEY (`case_id`),
    KEY `idx_email_eval_client_deleted` (`client_id`, `deleted`),
    KEY `idx_email_eval_scenario` (`scenario`),
    KEY `idx_email_eval_updated` (`updated_at`),
    CONSTRAINT `fk_email_eval_client`
        FOREIGN KEY (`client_id`) REFERENCES `client` (`client_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- Permisos del módulo (deben coincidir con Services/Permissions.cs).
INSERT IGNORE INTO `permission` (`code`, `description`) VALUES
    ('email-evaluation.read', 'Consultar el dataset de evaluación de correos'),
    ('email-evaluation.write', 'Crear, editar, eliminar y evaluar casos del dataset');

-- Lectura para Admin y Reader; escritura solo para Admin.
INSERT IGNORE INTO `profile_permission` (`profile_id`, `permission_id`)
SELECT p.profile_id, perm.permission_id
FROM `profile` p
INNER JOIN `permission` perm ON perm.code = 'email-evaluation.read'
WHERE p.name IN ('Admin', 'Reader');

INSERT IGNORE INTO `profile_permission` (`profile_id`, `permission_id`)
SELECT p.profile_id, perm.permission_id
FROM `profile` p
INNER JOIN `permission` perm ON perm.code = 'email-evaluation.write'
WHERE p.name = 'Admin';
