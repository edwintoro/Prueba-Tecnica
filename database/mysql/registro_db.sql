-- Base de datos única para todos los microservicios de registro-ms
-- Una sola BD, todas las tablas (mejor rendimiento, sin saltar entre bases)

SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

CREATE DATABASE IF NOT EXISTS registro_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE registro_db;

-- Programa de créditos
CREATE TABLE IF NOT EXISTS programas_creditos (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100) NOT NULL,
    creditos_por_materia INT NOT NULL DEFAULT 3,
    max_materias INT NOT NULL DEFAULT 3,
    total_creditos INT NOT NULL DEFAULT 9,
    activo TINYINT(1) NOT NULL DEFAULT 1,
    INDEX idx_programas_creditos_activo (activo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Profesores (5)
CREATE TABLE IF NOT EXISTS profesores (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100) NOT NULL,
    INDEX idx_profesores_nombre (nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Materias (10, 3 créditos c/u)
CREATE TABLE IF NOT EXISTS materias (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100) NOT NULL,
    creditos INT NOT NULL DEFAULT 3,
    profesor_id INT NOT NULL,
    programa_id INT NOT NULL,
    INDEX idx_materias_profesor_id (profesor_id),
    INDEX idx_materias_programa_id (programa_id),
    INDEX idx_materias_nombre (nombre),
    CONSTRAINT fk_materias_profesor FOREIGN KEY (profesor_id) REFERENCES profesores(id),
    CONSTRAINT fk_materias_programa FOREIGN KEY (programa_id) REFERENCES programas_creditos(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Estudiantes
CREATE TABLE IF NOT EXISTS estudiantes (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL,
    fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE INDEX uq_estudiantes_email (email),
    INDEX idx_estudiantes_nombre (nombre),
    INDEX idx_estudiantes_fecha_registro (fecha_registro)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Adhesión al programa de créditos
CREATE TABLE IF NOT EXISTS estudiante_programa (
    id INT PRIMARY KEY AUTO_INCREMENT,
    estudiante_id INT NOT NULL,
    programa_id INT NOT NULL,
    fecha_adhesion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE INDEX uq_estudiante_programa (estudiante_id, programa_id),
    INDEX idx_estudiante_programa_programa_id (programa_id),
    INDEX idx_estudiante_programa_fecha (fecha_adhesion),
    CONSTRAINT fk_ep_estudiante FOREIGN KEY (estudiante_id) REFERENCES estudiantes(id) ON DELETE CASCADE,
    CONSTRAINT fk_ep_programa FOREIGN KEY (programa_id) REFERENCES programas_creditos(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Inscripciones a materias
CREATE TABLE IF NOT EXISTS inscripciones (
    id INT PRIMARY KEY AUTO_INCREMENT,
    estudiante_id INT NOT NULL,
    materia_id INT NOT NULL,
    fecha_inscripcion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE INDEX uq_estudiante_materia (estudiante_id, materia_id),
    INDEX idx_inscripciones_materia_id (materia_id),
    INDEX idx_inscripciones_estudiante_fecha (estudiante_id, fecha_inscripcion),
    CONSTRAINT fk_insc_estudiante FOREIGN KEY (estudiante_id) REFERENCES estudiantes(id) ON DELETE CASCADE,
    CONSTRAINT fk_insc_materia FOREIGN KEY (materia_id) REFERENCES materias(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Datos iniciales
INSERT INTO programas_creditos (nombre, creditos_por_materia, max_materias, total_creditos, activo)
VALUES ('Programa Académico de Créditos', 3, 3, 9, 1);

INSERT INTO profesores (nombre) VALUES
('Prof. García'), ('Prof. López'), ('Prof. Martínez'), ('Prof. Rodríguez'), ('Prof. Hernández');

INSERT INTO materias (nombre, creditos, profesor_id, programa_id) VALUES
('Matemáticas',   3, 1, 1), ('Física',        3, 1, 1),
('Química',       3, 2, 1), ('Biología',      3, 2, 1),
('Programación',  3, 3, 1), ('Base de Datos', 3, 3, 1),
('Historia',      3, 4, 1), ('Geografía',     3, 4, 1),
('Inglés',        3, 5, 1), ('Literatura',    3, 5, 1);

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
(3, 'Secretaria', 'Secretaria')
ON DUPLICATE KEY UPDATE nombre = VALUES(nombre);
