-- El script 010 creó el permiso 'mail.scan' pero no lo asignó a ningún perfil,
-- por lo que los endpoints /api/mail/* devolvían 403 para todos los usuarios.
-- Se asigna al perfil Admin (igual que el resto de permisos, ver 008).
-- Idempotente: se puede ejecutar más de una vez sin error.

INSERT IGNORE INTO `permission` (`code`, `description`) VALUES
    ('mail.scan', 'Escanear el buzón IMAP y consultar resúmenes/comparativas de correo');

INSERT IGNORE INTO `profile_permission` (`profile_id`, `permission_id`)
SELECT p.profile_id, perm.permission_id
FROM `profile` p
INNER JOIN `permission` perm ON perm.code = 'mail.scan'
WHERE p.name = 'Admin';
