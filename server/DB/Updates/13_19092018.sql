USE `bomber_general`;

ALTER TABLE `bomber_general`.`member` 
ADD COLUMN `level` SMALLINT(6) UNSIGNED NOT NULL DEFAULT '1' AFTER `sex`,
ADD COLUMN `experience` BIGINT(20) UNSIGNED NOT NULL DEFAULT '0' AFTER `level`;

DROP PROCEDURE IF EXISTS `fn_auth`;
DROP PROCEDURE IF EXISTS `fn_fb_auth`;
DROP PROCEDURE IF EXISTS `fn_token_auth`;
# -------------------------------------------------------------------------------
# -------------------------------------------------------------------------------

USE `bomber_general`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `fn_auth`(p_login varchar(50), p_pass varchar(50), p_token varchar(512))
BEGIN
	IF (SELECT true FROM member WHERE login = p_login AND pass = p_pass) THEN
		BEGIN
			UPDATE member SET token = p_token WHERE login = p_login AND pass = p_pass;
			SELECT id, nick, sex, privilege, level, experience, first_login FROM member WHERE login = p_login AND pass = p_pass;
		END;
    END IF;
END
DELIMITER ;
# -------------------------------------------------------------------------------
# -------------------------------------------------------------------------------

USE `bomber_general`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `fn_fb_auth`(p_token varchar(512), p_id varchar(50), p_name varchar(50), p_email varchar(512))
BEGIN
	IF (SELECT true FROM member WHERE login = p_id) THEN
		BEGIN
			UPDATE member SET token = p_token WHERE login = p_id;
		END;
	ELSE
		BEGIN
			INSERT INTO member (login, pass, token, email) VALUES (p_id, '', p_token, email);
        END;
    END IF;
    
    SELECT id, coalesce(nick, '') as nick, sex, level, experience, privilege, first_login FROM member WHERE login = p_id;
END
DELIMITER ;
# -------------------------------------------------------------------------------
# -------------------------------------------------------------------------------

USE `bomber_general`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `fn_token_auth`(p_token varchar(512))
BEGIN
	SELECT id, login, nick, sex, level, experience, privilege, first_login FROM member WHERE token = p_token;
END
DELIMITER ;
# -------------------------------------------------------------------------------
# -------------------------------------------------------------------------------

USE `bomber_general`;