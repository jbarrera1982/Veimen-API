-- Nuevo permiso 'users.manage' (protege /api/users: administración de usuarios por el admin).
-- Debe coincidir con Services/Permissions.cs (Permissions.UsersManage).

INSERT INTO `permission` (`code`, `description`) VALUES
    ('users.manage', 'Administrar usuarios (crear, editar, resetear y eliminar)');

-- Asignarlo solo al perfil Admin.
INSERT INTO `profile_permission` (`profile_id`, `permission_id`)
SELECT p.profile_id, perm.permission_id
FROM `profile` p
INNER JOIN `permission` perm ON perm.code = 'users.manage'
WHERE p.name = 'Admin';
