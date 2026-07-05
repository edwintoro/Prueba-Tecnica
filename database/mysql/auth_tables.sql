-- Autenticación y autorización (roles, usuarios, sesiones)
CREATE TABLE IF NOT EXISTS roles (
    id INT PRIMARY KEY AUTO_INCREMENT,
    codigo VARCHAR(50) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    UNIQUE INDEX uq_roles_codigo (codigo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS usuarios (
    id INT PRIMARY KEY AUTO_INCREMENT,
    email VARCHAR(150) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    rol_id INT NOT NULL,
    estudiante_id INT NULL,
    activo TINYINT(1) NOT NULL DEFAULT 1,
    creado_en DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE INDEX uq_usuarios_email (email),
    INDEX idx_usuarios_rol_id (rol_id),
    INDEX idx_usuarios_estudiante_id (estudiante_id),
    CONSTRAINT fk_usuarios_rol FOREIGN KEY (rol_id) REFERENCES roles(id),
    CONSTRAINT fk_usuarios_estudiante FOREIGN KEY (estudiante_id) REFERENCES estudiantes(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS sesiones (
    id CHAR(36) PRIMARY KEY,
    usuario_id INT NOT NULL,
    refresh_token_hash VARCHAR(255) NOT NULL,
    ultima_actividad DATETIME NOT NULL,
    expira_en DATETIME NOT NULL,
    revocada TINYINT(1) NOT NULL DEFAULT 0,
    INDEX idx_sesiones_usuario_id (usuario_id),
    CONSTRAINT fk_sesiones_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO roles (id, codigo, nombre) VALUES
(1, 'Estudiante', 'Estudiante'),
(2, 'Profesor', 'Profesor'),
(3, 'Secretaria', 'Secretaria'),
(4, 'Administrador', 'Administrador')
ON DUPLICATE KEY UPDATE nombre = VALUES(nombre);
