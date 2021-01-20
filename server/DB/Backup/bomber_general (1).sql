-- MySQL dump 10.13  Distrib 8.0.23, for Win64 (x86_64)
--
-- Host: localhost    Database: bomber_general
-- ------------------------------------------------------
-- Server version	8.0.23

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
-- Table structure for table `cell_type`
--

DROP TABLE IF EXISTS `cell_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cell_type` (
  `code` int NOT NULL COMMENT 'code of corresponding enum CellType of GridEngine',
  `name` varchar(50) NOT NULL COMMENT 'name of cell type, to make easier identify',
  `attributes` int NOT NULL COMMENT 'flags of enum CellAttributes of the corresponding type',
  PRIMARY KEY (`code`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cell_type`
--

LOCK TABLES `cell_type` WRITE;
/*!40000 ALTER TABLE `cell_type` DISABLE KEYS */;
INSERT INTO `cell_type` VALUES (0,'NONE',0),(1,'INVISIBLE',1),(2,'PLANT',3),(3,'ROCK',1),(4,'WOODEN',3),(5,'ANVIL',1);
/*!40000 ALTER TABLE `cell_type` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `configuration`
--

DROP TABLE IF EXISTS `configuration`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configuration` (
  `name` varchar(50) NOT NULL COMMENT 'name of configuration, AKA key.',
  `value` varchar(50) NOT NULL COMMENT 'value of configuration.',
  `instance_id` int NOT NULL COMMENT 'unique identifier of instance which those infos care about.',
  PRIMARY KEY (`name`,`value`,`instance_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configuration`
--

LOCK TABLES `configuration` WRITE;
/*!40000 ALTER TABLE `configuration` DISABLE KEYS */;
INSERT INTO `configuration` VALUES ('capacity','2000',1),('dbServerAddress','127.0.0.1',1),('dbServerAddress','127.0.0.1',3),('dbServerPort','11510',1),('dbServerPort','11510',3),('fbAppID','483331038836128',3),('fbAppSecret','fdc3989e29dbba85cee13aa35fffba50',3),('fbRedirectUri','https://google.com.br',3),('lbServerAddress','127.0.0.1',1),('lbServerPort','11511',1),('listenAddress','0.0.0.0',1),('listenAddress','0.0.0.0',2),('listenAddress','0.0.0.0',3),('listenPort','11510',2),('listenPort','9875',3),('listenPort','9876',1),('playerReconnectTimeout','60',3),('publicAddress','127.0.0.1',1),('rmListenAddress','0.0.0.0',3),('rmListenPort','11511',3);
/*!40000 ALTER TABLE `configuration` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `friendship`
--

DROP TABLE IF EXISTS `friendship`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `friendship` (
  `id` bigint unsigned NOT NULL,
  `friend_id` bigint NOT NULL,
  `register_date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`,`friend_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `friendship`
--

LOCK TABLES `friendship` WRITE;
/*!40000 ALTER TABLE `friendship` DISABLE KEYS */;
/*!40000 ALTER TABLE `friendship` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `friendship_request`
--

DROP TABLE IF EXISTS `friendship_request`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `friendship_request` (
  `id` bigint unsigned NOT NULL,
  `friend_id` bigint NOT NULL,
  `register_date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`,`friend_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `friendship_request`
--

LOCK TABLES `friendship_request` WRITE;
/*!40000 ALTER TABLE `friendship_request` DISABLE KEYS */;
/*!40000 ALTER TABLE `friendship_request` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `map`
--

DROP TABLE IF EXISTS `map`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `map` (
  `id` int NOT NULL AUTO_INCREMENT COMMENT 'MAP unique identifier',
  `name` varchar(50) NOT NULL COMMENT 'name of map, just to make easier to identify',
  `width` int NOT NULL COMMENT 'number of `blocks` this map have on X axis',
  `height` int NOT NULL COMMENT 'number of `blocks`this map have on Y axis',
  `player_cnt` int NOT NULL DEFAULT '0',
  `data` varbinary(40000) NOT NULL,
  `background` int NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `map`
--

LOCK TABLES `map` WRITE;
/*!40000 ALTER TABLE `map` DISABLE KEYS */;
INSERT INTO `map` VALUES (1,'NoNameYet',19,13,6,_binary '		\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0',1);
/*!40000 ALTER TABLE `map` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `map_behaviour`
--

DROP TABLE IF EXISTS `map_behaviour`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `map_behaviour` (
  `id` int NOT NULL AUTO_INCREMENT COMMENT 'unique identify of powerup item',
  `map_id` int NOT NULL COMMENT 'the foreign key that references the powerup affected by this behaviour',
  `behaviour` varchar(50) NOT NULL COMMENT 'name of behaviour of this powerup. Used to instanciate behaviour class',
  `settings` json DEFAULT NULL COMMENT 'the json settings used on this item behaviour',
  PRIMARY KEY (`id`),
  KEY `fk_map_behaviour_map_id` (`map_id`),
  CONSTRAINT `fk_map_behaviour_map_id` FOREIGN KEY (`map_id`) REFERENCES `map` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `map_behaviour`
--

LOCK TABLES `map_behaviour` WRITE;
/*!40000 ALTER TABLE `map_behaviour` DISABLE KEYS */;
INSERT INTO `map_behaviour` VALUES (1,1,'Classic','{\"hurryupTime\": 99999}');
/*!40000 ALTER TABLE `map_behaviour` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `member`
--

DROP TABLE IF EXISTS `member`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `member` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `register_date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `login` varchar(20) NOT NULL,
  `pass` varchar(50) NOT NULL,
  `token` varbinary(256) DEFAULT NULL,
  `nick` varchar(16) DEFAULT NULL,
  `sex` tinyint(1) NOT NULL DEFAULT '0',
  `level` smallint unsigned NOT NULL DEFAULT '1',
  `experience` bigint unsigned NOT NULL DEFAULT '0',
  `privilege` tinyint NOT NULL DEFAULT '0',
  `first_login` tinyint NOT NULL DEFAULT '1',
  `email` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id` (`id`),
  UNIQUE KEY `login` (`login`)
) ENGINE=InnoDB AUTO_INCREMENT=26 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `member`
--

LOCK TABLES `member` WRITE;
/*!40000 ALTER TABLE `member` DISABLE KEYS */;
/*!40000 ALTER TABLE `member` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `powerup`
--

DROP TABLE IF EXISTS `powerup`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `powerup` (
  `id` int NOT NULL AUTO_INCREMENT COMMENT 'unique identify of powerup item',
  `name` varchar(50) NOT NULL COMMENT 'name of powerup, to make easier identify',
  `icon` int NOT NULL COMMENT 'id of icon used by this powerup',
  `rate` decimal(5,2) NOT NULL COMMENT 'the rate of this item appear when a block is broken',
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `powerup`
--

LOCK TABLES `powerup` WRITE;
/*!40000 ALTER TABLE `powerup` DISABLE KEYS */;
INSERT INTO `powerup` VALUES (1,'Bomb Area',1,7.00),(2,'Bomb Quantity',2,6.00),(3,'Move Speed',3,5.00),(4,'Extra Life',4,3.00),(5,'Temp immunity',5,1.00),(6,'Bomb Kick',6,4.00);
/*!40000 ALTER TABLE `powerup` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `powerup_behaviour`
--

DROP TABLE IF EXISTS `powerup_behaviour`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `powerup_behaviour` (
  `id` int NOT NULL AUTO_INCREMENT COMMENT 'unique identify of powerup item',
  `powerup_id` int NOT NULL COMMENT 'the foreign key that references the powerup affected by this behaviour',
  `behaviour` varchar(50) NOT NULL COMMENT 'name of behaviour of this powerup. Used to instanciate behaviour class',
  `settings` json DEFAULT NULL COMMENT 'the json settings used on this item behaviour',
  PRIMARY KEY (`id`),
  KEY `fk_powerup_behaviour_powerup_id` (`powerup_id`),
  CONSTRAINT `fk_powerup_behaviour_powerup_id` FOREIGN KEY (`powerup_id`) REFERENCES `powerup` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `powerup_behaviour`
--

LOCK TABLES `powerup_behaviour` WRITE;
/*!40000 ALTER TABLE `powerup_behaviour` DISABLE KEYS */;
INSERT INTO `powerup_behaviour` VALUES (1,1,'IncBombArea',NULL),(2,2,'IncBombQtt',NULL),(3,3,'IncMoveSpeed',NULL),(4,4,'IncLife',NULL),(5,5,'TempImmunity','{\"duration\": 5}'),(6,6,'IncBombKick',NULL);
/*!40000 ALTER TABLE `powerup_behaviour` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping events for database 'bomber_general'
--

--
-- Dumping routines for database 'bomber_general'
--
/*!50003 DROP PROCEDURE IF EXISTS `remove_friendship` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'NO_AUTO_VALUE_ON_ZERO' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `remove_friendship`(p_nick varchar(50), p_friend varchar(50))
proc_root:BEGIN
	DECLARE user_id BIGINT DEFAULT 0;
    DECLARE friend_id BIGINT DEFAULT 0;

	SELECT id INTO user_id FROM member WHERE nick = p_nick;

	IF (user_id = 0) THEN
		SELECT -1; # Invalid requester ID
        LEAVE proc_root;
    END IF;

	SELECT id INTO friend_id FROM member WHERE nick = p_friend;

	IF (friend_id = 0) THEN
		SELECT -2; # Invalid requested ID
        LEAVE proc_root;
    END IF;

	IF NOT (SELECT true FROM friendship WHERE (friend_id = friend_id AND id = user_id) OR (id = friend_id AND friend_id = user_id)) THEN
		SELECT -3; # They are not friends
        LEAVE proc_root;
    END IF;
    
    DELETE FROM friendship WHERE (friend_id = friend_id AND id = user_id) OR (id = friend_id AND friend_id = user_id); # Delete friendship in both sides
    
    SELECT 1;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `request_friendship` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'NO_AUTO_VALUE_ON_ZERO' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `request_friendship`(p_requester varchar(50), p_requested varchar(50))
proc_root:BEGIN
	DECLARE requester_id BIGINT DEFAULT 0;
    DECLARE requested_id BIGINT DEFAULT 0;

	SELECT id INTO requester_id FROM member WHERE nick = p_requester;

	IF (requester_id = 0) THEN
		SELECT -1, null; # Invalid requester ID
        LEAVE proc_root;
    END IF;

	SELECT id INTO requested_id FROM member WHERE nick = p_requested;

	IF (requested_id = 0) THEN
		SELECT -2, null; # Invalid requested ID
        LEAVE proc_root;
    END IF;

	IF (SELECT true FROM friendship_request WHERE id = requester_id AND friend_id = requested_id) THEN
		SELECT -3, null; # There is already a friend request from this requester to the requested one
        LEAVE proc_root;
    END IF;
    
    IF (SELECT true FROM friendship_request WHERE friend_id = requester_id AND id = requested_id) THEN
		SELECT -4, null; # There is already a friend request from this requested one to the requester
        LEAVE proc_root;
    END IF;
    
	IF (SELECT true FROM friendship WHERE (friend_id = requester_id AND id = requested_id) OR (id = requester_id AND friend_id = requested_id)) THEN
		SELECT -5, null; # They are already friends
        LEAVE proc_root;
    END IF;
    
    INSERT INTO friendship_request (id, friend_id) VALUES (requester_id, requested_id);
    
    SELECT requested_id, login FROM member WHERE id = requested_id; # Return index and login
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `response_friendship` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'NO_AUTO_VALUE_ON_ZERO' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `response_friendship`(p_requester varchar(50), p_requested varchar(50), p_accepted boolean)
proc_root:BEGIN
	DECLARE requester_id BIGINT DEFAULT 0;
    DECLARE requested_id BIGINT DEFAULT 0;

	SELECT id INTO requester_id FROM member WHERE nick = p_requester;

	IF (requester_id = 0) THEN
		SELECT -1, null; # Invalid requester ID
        LEAVE proc_root;
    END IF;

	SELECT id INTO requested_id FROM member WHERE nick = p_requested;

	IF (requested_id = 0) THEN
		SELECT -2, null; # Invalid requested ID
        LEAVE proc_root;
    END IF;
#tele opti - 
	IF NOT (SELECT true FROM friendship_request WHERE id = requester_id AND friend_id = requested_id) THEN
		SELECT -3, null; # There is not friend request from this requester to the requested one
        LEAVE proc_root;
    END IF;
    
    IF (SELECT true FROM friendship WHERE (friend_id = requester_id AND id = requested_id) OR (id = requester_id AND friend_id = requested_id)) THEN
		SELECT -4, null; # They are already friends
        LEAVE proc_root;
    END IF;
    
    DELETE FROM friendship_request WHERE id = requester_id AND friend_id = requested_id; # Remove any request from both sides
    DELETE FROM friendship_request WHERE friend_id = requester_id AND id = requested_id;
    
	IF (p_accepted) THEN
		INSERT INTO friendship (id, friend_id) VALUES (requester_id, requested_id); # Add both as friend
		INSERT INTO friendship (id, friend_id) VALUES (requested_id, requester_id);
    END IF;
    
    SELECT requester_id, login FROM member WHERE id = requester_id; # Return index and login from the requester
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-01-20 12:32:05
