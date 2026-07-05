-- Base de datos única PostgreSQL para registro-ms (con índices)

CREATE DATABASE registro_db;
\c registro_db;

CREATE TABLE IF NOT EXISTS programas_creditos (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    creditos_por_materia INT NOT NULL DEFAULT 3,
    max_materias INT NOT NULL DEFAULT 3,
    total_creditos INT NOT NULL DEFAULT 9,
    activo BOOLEAN NOT NULL DEFAULT TRUE
);
CREATE INDEX idx_programas_creditos_activo ON programas_creditos (activo);

CREATE TABLE IF NOT EXISTS profesores (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL
);
CREATE INDEX idx_profesores_nombre ON profesores (nombre);

CREATE TABLE IF NOT EXISTS materias (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    creditos INT NOT NULL DEFAULT 3,
    profesor_id INT NOT NULL REFERENCES profesores(id),
    programa_id INT NOT NULL REFERENCES programas_creditos(id)
);
CREATE INDEX idx_materias_profesor_id ON materias (profesor_id);
CREATE INDEX idx_materias_programa_id ON materias (programa_id);
CREATE INDEX idx_materias_nombre ON materias (nombre);

CREATE TABLE IF NOT EXISTS estudiantes (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    fecha_registro TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX idx_estudiantes_nombre ON estudiantes (nombre);
CREATE INDEX idx_estudiantes_fecha_registro ON estudiantes (fecha_registro);

CREATE TABLE IF NOT EXISTS estudiante_programa (
    id SERIAL PRIMARY KEY,
    estudiante_id INT NOT NULL REFERENCES estudiantes(id) ON DELETE CASCADE,
    programa_id INT NOT NULL REFERENCES programas_creditos(id),
    fecha_adhesion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (estudiante_id, programa_id)
);
CREATE INDEX idx_estudiante_programa_programa_id ON estudiante_programa (programa_id);
CREATE INDEX idx_estudiante_programa_fecha ON estudiante_programa (fecha_adhesion);

CREATE TABLE IF NOT EXISTS inscripciones (
    id SERIAL PRIMARY KEY,
    estudiante_id INT NOT NULL REFERENCES estudiantes(id) ON DELETE CASCADE,
    materia_id INT NOT NULL REFERENCES materias(id),
    fecha_inscripcion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (estudiante_id, materia_id)
);
CREATE INDEX idx_inscripciones_materia_id ON inscripciones (materia_id);
CREATE INDEX idx_inscripciones_estudiante_fecha ON inscripciones (estudiante_id, fecha_inscripcion);

INSERT INTO programas_creditos (nombre, creditos_por_materia, max_materias, total_creditos, activo)
VALUES ('Programa Académico de Créditos', 3, 3, 9, TRUE);

INSERT INTO profesores (nombre) VALUES
('Prof. García'), ('Prof. López'), ('Prof. Martínez'), ('Prof. Rodríguez'), ('Prof. Hernández');

INSERT INTO materias (nombre, creditos, profesor_id, programa_id) VALUES
('Matemáticas',   3, 1, 1), ('Física',        3, 1, 1),
('Química',       3, 2, 1), ('Biología',      3, 2, 1),
('Programación',  3, 3, 1), ('Base de Datos', 3, 3, 1),
('Historia',      3, 4, 1), ('Geografía',     3, 4, 1),
('Inglés',        3, 5, 1), ('Literatura',    3, 5, 1);
