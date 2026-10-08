-- Permisos del módulo Clientes (CRUD /api/clients).
-- Deben coincidir con Services/Permissions.cs (Permissions.ClientsRead / ClientsWrite).

INSERT INTO `permission` (`code`, `description`) VALUES
    ('clients.read', 'Consultar clientes'),
    ('clients.write', 'Crear, editar y eliminar clientes');

-- clients.read para Admin y Reader; clients.write solo para Admin.
INSERT INTO `profile_permission` (`profile_id`, `permission_id`)
SELECT p.profile_id, perm.permission_id
FROM `profile` p
INNER JOIN `permission` perm ON perm.code = 'clients.read'
WHERE p.name IN ('Admin', 'Reader');

INSERT INTO `profile_permission` (`profile_id`, `permission_id`)
SELECT p.profile_id, perm.permission_id
FROM `profile` p
INNER JOIN `permission` perm ON perm.code = 'clients.write'
WHERE p.name = 'Admin';
