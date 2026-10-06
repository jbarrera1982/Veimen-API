-- Nuevo permiso 'usage.read' (protege /api/usage/costs: consulta de costos en la API de OpenAI).
-- Debe coincidir con Services/Permissions.cs (Permissions.UsageRead).

INSERT INTO `permission` (`code`, `description`) VALUES
    ('usage.read', 'Consultar costos de uso de OpenAI');

-- Asignarlo solo al perfil Admin.
INSERT INTO `profile_permission` (`profile_id`, `permission_id`)
SELECT p.profile_id, perm.permission_id
FROM `profile` p
INNER JOIN `permission` perm ON perm.code = 'usage.read'
WHERE p.name = 'Admin';
