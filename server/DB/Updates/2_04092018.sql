DROP TABLE IF EXISTS friendship;
CREATE TABLE friendship (`id` bigint(20) unsigned NOT NULL, `friend_id` bigint(20) NOT NULL, `register_date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY (`id`, `friend_id`));

DROP TABLE IF EXISTS friendship_request;
CREATE TABLE friendship_request (`id` bigint(20) unsigned NOT NULL, `friend_id` bigint(20) NOT NULL, `register_date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY (`id`, `friend_id`));

DROP PROCEDURE IF EXISTS `remove_friendship`;

DELIMITER $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `remove_friendship`(p_nick varchar(50), p_friend varchar(50))
proc_root:BEGIN
	DECLARE v_user_id BIGINT DEFAULT 0;
    DECLARE v_friend_id BIGINT DEFAULT 0;

	SELECT id INTO v_user_id FROM member WHERE nick = p_nick;

	IF (v_user_id = 0) THEN
		SELECT -1; # Invalid requester ID
        LEAVE proc_root;
    END IF;

	SELECT id INTO v_friend_id FROM member WHERE nick = p_friend;

	IF (v_friend_id = 0) THEN
		SELECT -2; # Invalid requested ID
        LEAVE proc_root;
    END IF;

	#If there is no friendship
	IF NOT EXISTS(SELECT true FROM friendship WHERE (friend_id = v_friend_id AND id = v_user_id) OR (id = v_friend_id AND friend_id = v_user_id)) THEN
    
		#If there is no friendship request
		IF NOT EXISTS (SELECT true from friendship_request WHERE (friend_id = v_friend_id AND id = v_user_id) OR (id = v_friend_id AND friend_id = v_user_id)) THEN 
			SELECT -3; # They are not friends
			LEAVE proc_root;
		END IF;
        
        DELETE FROM friendship_request WHERE (friend_id = v_friend_id AND id = v_user_id) OR (id = v_friend_id AND friend_id = v_user_id); # Delete friendship request in both sides
    END IF;
    
    DELETE FROM friendship WHERE (friend_id = v_friend_id AND id = v_user_id) OR (id = v_friend_id AND friend_id = v_user_id); # Delete friendship in both sides
    
    SELECT 1;
    
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `response_friendship`;

DELIMITER $$
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
	IF NOT EXISTS (SELECT true FROM friendship_request WHERE id = requester_id AND friend_id = requested_id) THEN
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
END$$
DELIMITER ;