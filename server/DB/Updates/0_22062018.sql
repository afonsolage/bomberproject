ALTER TABLE member ADD COLUMN token VARBINARY(256) DEFAULT NULL AFTER pass;

DELIMITER //
CREATE PROCEDURE fn_auth(p_login varchar(50), p_pass varchar(50), p_token varchar(512))
BEGIN
	IF (SELECT true FROM member WHERE login = p_login AND pass = p_pass) THEN
		BEGIN
			UPDATE member SET token = p_token WHERE login = p_login AND pass = p_pass;
			SELECT id, nick FROM member WHERE login = p_login AND pass = p_pass;
		END;
    END IF;
END; //
DELIMITER ;

DELIMITER //
CREATE PROCEDURE fn_token_auth(p_token varchar(512))
BEGIN
	SELECT id, login, nick FROM member WHERE token = p_token;
END; //
DELIMITER ;