-- Nuevo permiso 'trace.write' (permite registrar una revisión manual: crea una traza
-- en service_request_trace y cambia el estado del requerimiento a Rejected/Closed).
-- Debe coincidir con Services/Permissions.cs (Permissions.TraceWrite).

INSERT INTO `permission` (`code`, `description`) VALUES
    ('trace.write', 'Registrar revisiones manuales de requerimientos (traza + cambio de estado)');

-- trace.write solo para Admin (es una escritura sobre datos de negocio).
INSERT INTO `profile_permission` (`profile_id`, `permission_id`)
SELECT p.profile_id, perm.permission_id
FROM `profile` p
INNER JOIN `permission` perm ON perm.code = 'trace.write'
WHERE p.name = 'Admin';
