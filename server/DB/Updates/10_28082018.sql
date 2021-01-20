CREATE TABLE friendship (`id` bigint(20) unsigned NOT NULL AUTO_INCREMENT PRIMARY KEY, `friend_id` bigint(20) NOT NULL, `register_date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE friendship_request (`id` bigint(20) unsigned NOT NULL AUTO_INCREMENT PRIMARY KEY, `friend_id` bigint(20) NOT NULL, `register_date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP);

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `request_friendship`(p_requester varchar(50), p_requested varchar(50))
proc_root:BEGIN
	DECLARE requester_id BIGINT DEFAULT 0;
    DECLARE requested_id BIGINT DEFAULT 0;

	SELECT id INTO requester_id FROM member WHERE login = p_requester;

	IF (requester_id = 0) THEN
		SELECT -1; # Invalid requester ID
        LEAVE proc_root;
    END IF;

	SELECT id INTO requested_id FROM member WHERE login = p_requested;

	IF (requested_id = 0) THEN
		SELECT -2; # Invalid requested ID
        LEAVE proc_root;
    END IF;

	IF (SELECT true FROM friendship_request WHERE id = requester_id AND friend_id = requested_id) THEN
		SELECT -3; # There is already a friend request from this requester to the requested one
        LEAVE proc_root;
    END IF;
    
    IF (SELECT true FROM friendship_request WHERE friend_id = requester_id AND id = requested_id) THEN
		SELECT -4; # There is already a friend request from this requested one to the requester
        LEAVE proc_root;
    END IF;
    
	IF (SELECT true FROM friendship WHERE (friend_id = requester_id AND id = requested_id) OR (id = requester_id AND friend_id = requested_id)) THEN
		SELECT -5; # They are already friends
        LEAVE proc_root;
    END IF;
    
    INSERT INTO friendship_request (id, friend_id) VALUES (requester_id, requested_id);
    SELECT 1;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `response_friendship`(p_requester varchar(50), p_requested varchar(50), p_accepted boolean)
proc_root:BEGIN
	DECLARE requester_id BIGINT DEFAULT 0;
    DECLARE requested_id BIGINT DEFAULT 0;

	SELECT id INTO requester_id FROM member WHERE login = p_requester;

	IF (requester_id = 0) THEN
		SELECT -1; # Invalid requester ID
        LEAVE proc_root;
    END IF;

	SELECT id INTO requested_id FROM member WHERE login = p_requested;

	IF (requested_id = 0) THEN
		SELECT -2; # Invalid requested ID
        LEAVE proc_root;
    END IF;

	IF NOT (SELECT true FROM friendship_request WHERE id = requester_id AND friend_id = requested_id) THEN
		SELECT -3; # There is not friend request from this requester to the requested one
        LEAVE proc_root;
    END IF;
    
    IF (SELECT true FROM friendship WHERE (friend_id = requester_id AND id = requested_id) OR (id = requester_id AND friend_id = requested_id)) THEN
		SELECT -4; # They are already friends
        LEAVE proc_root;
    END IF;
    
    DELETE FROM friendship_request WHERE id = requester_id AND friend_id = requested_id; # Remove any request from both sides
    DELETE FROM friendship_request WHERE friend_id = requester_id AND id = requested_id;
    
	IF (p_accepted) THEN
		INSERT INTO friendship (id, friend_id) VALUES (requester_id, requested_id); # Add both as friend
		INSERT INTO friendship (id, friend_id) VALUES (requested_id, requester_id);
    END IF;
    
    SELECT 1;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `remove_friendship`(p_login varchar(50), p_friend varchar(50))
proc_root:BEGIN
	DECLARE user_id BIGINT DEFAULT 0;
    DECLARE friend_id BIGINT DEFAULT 0;

	SELECT id INTO user_id FROM member WHERE login = p_login;

	IF (user_id = 0) THEN
		SELECT -1; # Invalid requester ID
        LEAVE proc_root;
    END IF;

	SELECT id INTO friend_id FROM member WHERE login = p_friend;

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
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `list_friendship`(p_id bigint)
proc_root:BEGIN
	SELECT m.id, m.login, m.nick, 0 as state FROM member M INNER JOIN friendship F ON F.friend_id = M.id WHERE F.id = p_id # Friends
    UNION
    SELECT m.id, m.login, m.nick, 1 as state FROM member M INNER JOIN friendship_request RF ON RF.friend_id = M.id WHERE RF.id = p_id # Friends requested.
    UNION
    SELECT m.id, m.login, m.nick, 2 as state FROM member M INNER JOIN friendship_request RF ON RF.id = M.id WHERE RF.friend_id = p_id; # Friends waiting to be accepted.
END$$
DELIMITER ;