use bomber_general;

DELIMITER $$
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
    
    SELECT id, coalesce(nick, '') as nick, sex, privilege, first_login FROM member WHERE login = p_id;
END$$
DELIMITER ;