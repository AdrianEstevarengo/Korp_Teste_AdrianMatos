-- Cria os bancos de cada microsserviço (database-per-service).
-- Executado automaticamente pelo container do Postgres na primeira subida.
CREATE DATABASE db_estoque;
CREATE DATABASE db_faturamento;
