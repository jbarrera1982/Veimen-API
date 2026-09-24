-- Nuevo permiso 'dashboard.read' (protege GET /api/ServiceRequests/dashboard).
-- Debe coincidir con Services/Permissions.cs (Permissions.DashboardRead).

INSERT INTO `permission` (`code`, `description`) VALUES
    ('dashboard.read', 'Consultar el dashboard de service requests');

-- Asignarlo a los perfiles existentes (ajustar la lista según necesidad).
INSERT INTO `profile_permission` (`profile_id`, `permission_id`)
SELECT p.profile_id, perm.permission_id
FROM `profile` p
INNER JOIN `permission` perm ON perm.code = 'dashboard.read'
WHERE p.name IN ('Admin', 'Reader');
