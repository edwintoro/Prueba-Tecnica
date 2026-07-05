-- Corrige datos con acentos (ejecutar si aparecen caracteres como Ã© en la app)
SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

USE registro_db;

SET FOREIGN_KEY_CHECKS = 0;
TRUNCATE TABLE inscripciones;
TRUNCATE TABLE estudiante_programa;
TRUNCATE TABLE estudiantes;
TRUNCATE TABLE materias;
TRUNCATE TABLE profesores;
TRUNCATE TABLE programas_creditos;
SET FOREIGN_KEY_CHECKS = 1;

INSERT INTO programas_creditos (id, nombre, creditos_por_materia, max_materias, total_creditos, activo)
VALUES (1, 'Programa Académico de Créditos', 3, 3, 9, 1);

INSERT INTO profesores (id, nombre) VALUES
(1, 'Prof. García'), (2, 'Prof. López'), (3, 'Prof. Martínez'), (4, 'Prof. Rodríguez'), (5, 'Prof. Hernández');

INSERT INTO materias (nombre, creditos, profesor_id, programa_id) VALUES
('Matemáticas',   3, 1, 1), ('Física',        3, 1, 1),
('Química',       3, 2, 1), ('Biología',      3, 2, 1),
('Programación',  3, 3, 1), ('Base de Datos', 3, 3, 1),
('Historia',      3, 4, 1), ('Geografía',     3, 4, 1),
('Inglés',        3, 5, 1), ('Literatura',    3, 5, 1);
