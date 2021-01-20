USE bomber_general;

DROP PROCEDURE IF EXISTS `fn_auth`;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `fn_auth`(p_login varchar(50), p_pass varchar(50), p_token varchar(512))
BEGIN
	IF (SELECT true FROM member WHERE login = p_login AND pass = p_pass) THEN
		BEGIN
			UPDATE member SET token = p_token WHERE login = p_login AND pass = p_pass;
			SELECT id, coalesce(nick, '') as nick, sex, privilege, first_login FROM member WHERE login = p_login AND pass = p_pass;
		END;
    END IF;
END$$
DELIMITER ;

ALTER TABLE `bomber_general`.`member` CHANGE COLUMN `first_login` `first_login` TINYINT(4) NOT NULL DEFAULT '1' ;
