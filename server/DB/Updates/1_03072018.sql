ALTER TABLE `bomber_general`.`member` 
ADD COLUMN `sex` TINYINT(1) NOT NULL DEFAULT 0 AFTER `nick`;

---------------------------------------------------------------
---------------------------------------------------------------

USE `bomber_general`;
DROP procedure IF EXISTS `fn_auth`;

DELIMITER $$
USE `bomber_general`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `fn_auth`(p_login varchar(50), p_pass varchar(50), p_token varchar(512))
BEGIN
	IF (SELECT true FROM member WHERE login = p_login AND pass = p_pass) THEN
		BEGIN
			UPDATE member SET token = p_token WHERE login = p_login AND pass = p_pass;
			SELECT id, nick, sex FROM member WHERE login = p_login AND pass = p_pass;
		END;
    END IF;
END$$

DELIMITER ;

---------------------------------------------------------------
---------------------------------------------------------------

USE `bomber_general`;
DROP procedure IF EXISTS `fn_token_auth`;

DELIMITER $$
USE `bomber_general`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `fn_token_auth`(p_token varchar(512))
BEGIN
	SELECT id, login, nick, sex FROM member WHERE token = p_token;
END$$

DELIMITER ;
