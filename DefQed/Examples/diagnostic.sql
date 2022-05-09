CREATE DATABASE  IF NOT EXISTS `defqed` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `defqed`;
-- MySQL dump 10.13  Distrib 8.0.27, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: defqed
-- ------------------------------------------------------
-- Server version	8.0.27

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `notations`
--

DROP TABLE IF EXISTS `notations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `notations` (
  `ID` int NOT NULL,
  `TITLE` varchar(50) NOT NULL,
  `ORIGIN` tinyint NOT NULL,
  `OPACITY` double NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notations`
--

LOCK TABLES `notations` WRITE;
/*!40000 ALTER TABLE `notations` DISABLE KEYS */;
INSERT INTO `notations` VALUES (0,'item',1,1),(1,'==',0,1),(2,'AND',0,1);
/*!40000 ALTER TABLE `notations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `reflections`
--

DROP TABLE IF EXISTS `reflections`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reflections` (
  `ID` int NOT NULL,
  `CASES` bigint NOT NULL,
  `THUSES` bigint NOT NULL,
  `OPACITY` double NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reflections`
--

LOCK TABLES `reflections` WRITE;
/*!40000 ALTER TABLE `reflections` DISABLE KEYS */;
INSERT INTO `reflections` VALUES (0,0,1,1);
/*!40000 ALTER TABLE `reflections` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `registries`
--

DROP TABLE IF EXISTS `registries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `registries` (
  `ID` int NOT NULL,
  `CONTENT` longtext NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `registries`
--

LOCK TABLES `registries` WRITE;
/*!40000 ALTER TABLE `registries` DISABLE KEYS */;
INSERT INTO `registries` VALUES (0,'{\"TopLevel\":{\"MicroStatement\":null,\"Symbol\":null,\"Connector\":{\"Name\":\"AND\",\"Id\":2,\"Origin\":0},\"SubBrackets\":[{\"MicroStatement\":{\"Brackets\":[{\"MicroStatement\":null,\"Symbol\":{\"Name\":\"a\",\"Id\":0,\"Notation\":{\"Name\":\"item\",\"Id\":0,\"Origin\":1},\"DirectValue\":null},\"Connector\":null,\"SubBrackets\":[null,null],\"BracketType\":3,\"Satisfied\":0},{\"MicroStatement\":null,\"Symbol\":{\"Name\":\"b\",\"Id\":1,\"Notation\":{\"Name\":\"item\",\"Id\":0,\"Origin\":1},\"DirectValue\":null},\"Connector\":null,\"SubBrackets\":[null,null],\"BracketType\":3,\"Satisfied\":0}],\"Connector\":{\"Name\":\"==\",\"Id\":1,\"Origin\":0}},\"Symbol\":null,\"Connector\":null,\"SubBrackets\":[null,null],\"BracketType\":2,\"Satisfied\":0},{\"MicroStatement\":{\"Brackets\":[{\"MicroStatement\":null,\"Symbol\":{\"Name\":\"b\",\"Id\":1,\"Notation\":{\"Name\":\"item\",\"Id\":1,\"Origin\":1},\"DirectValue\":null},\"Connector\":null,\"SubBrackets\":[null,null],\"BracketType\":3,\"Satisfied\":0},{\"MicroStatement\":null,\"Symbol\":{\"Name\":\"c\",\"Id\":2,\"Notation\":{\"Name\":\"item\",\"Id\":2,\"Origin\":1},\"DirectValue\":null},\"Connector\":null,\"SubBrackets\":[null,null],\"BracketType\":3,\"Satisfied\":0}],\"Connector\":{\"Name\":\"==\",\"Id\":1,\"Origin\":0}},\"Symbol\":null,\"Connector\":null,\"SubBrackets\":[null,null],\"BracketType\":2,\"Satisfied\":0}],\"BracketType\":0,\"Satisfied\":0}}'),(1,'[{\"Brackets\":[{\"MicroStatement\":null,\"Symbol\":{\"Name\":\"a\",\"Id\":0,\"Notation\":{\"Name\":\"item\",\"Id\":0,\"Origin\":1},\"DirectValue\":null},\"Connector\":null,\"SubBrackets\":[null,null],\"BracketType\":3,\"Satisfied\":0},{\"MicroStatement\":null,\"Symbol\":{\"Name\":\"c\",\"Id\":2,\"Notation\":{\"Name\":\"item\",\"Id\":2,\"Origin\":1},\"DirectValue\":null},\"Connector\":null,\"SubBrackets\":[null,null],\"BracketType\":3,\"Satisfied\":0}],\"Connector\":{\"Name\":\"==\",\"Id\":1,\"Origin\":0}}]');
/*!40000 ALTER TABLE `registries` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2022-05-09 20:09:26
