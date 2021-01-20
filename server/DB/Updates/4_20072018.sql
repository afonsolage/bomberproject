USE `bomber_general`;
DROP TABLE IF EXISTS `member`;

--
-- Table structure for table `member`
--

CREATE TABLE `member` (
   `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
   `register_date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
   `login` varchar(20) NOT NULL,
   `pass` varchar(50) NOT NULL,
   `token` varbinary(256) DEFAULT NULL,
   `nick` varchar(16) DEFAULT NULL,
   `sex` tinyint(1) NOT NULL DEFAULT '0',
   `privilege` tinyint(4) NOT NULL DEFAULT '0',
   `first_login` tinyint(4) NOT NULL DEFAULT '1',
   `email` varchar(100) DEFAULT NULL,
   PRIMARY KEY (`id`),
   UNIQUE KEY `id` (`id`),
   UNIQUE KEY `login` (`login`)
 ) ENGINE=InnoDB AUTO_INCREMENT=26 DEFAULT CHARSET=utf8