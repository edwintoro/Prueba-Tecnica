-- Rol Administrador y usuario admin de prueba
INSERT INTO roles (id, codigo, nombre) VALUES
(4, 'Administrador', 'Administrador')
ON DUPLICATE KEY UPDATE nombre = VALUES(nombre);

-- admin@registro.edu / 123456
INSERT INTO usuarios (email, password_hash, rol_id, estudiante_id, activo)
SELECT 'admin@registro.edu',
       '$2a$11$e3myagTOYV/7MQ0bduEVIeUCfU8B6VZqvYl8Zoceda3/47/v.v5Fy',
       r.id,
       NULL,
       1
FROM roles r
WHERE r.codigo = 'Administrador'
  AND NOT EXISTS (SELECT 1 FROM usuarios u WHERE u.email = 'admin@registro.edu');
