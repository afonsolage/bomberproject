
namespace CommonLib.Messaging
{
    public enum MessageType
    {
        // SUFIXES:
        // - NFY => Notify - Means a notifications only, without requiring response.
        // - REQ => Request - Means a request to do, which will provide a corresponding response.
        // - RES => Response - Means a response from a previous request.

        // Client <-> LobbyServer - Should have CL prefix.
        // PS: For some reason, ProtoBuf won't serialize enum with value = 0, so set first value as 1.
        CL_WELCOME_NFY = 1, 

        CL_AUTH_REQ,
        CL_AUTH_RES,

        CL_LOGOUT_REQ,

        CL_FB_AUTH_REQ,
        CL_FB_AUTH_RES,

        CL_REGISTER_REQ,
        CL_REGISTER_RES,

        // Heartbeat message
        CL_PLAYER_HEARTBEAT_REQ,
        CL_PLAYER_HEARTBEAT_RES,
        CL_PLAYER_HEARTBEAT_NFY,

        CL_PLAYER_CREATE_REQ,
        CL_PLAYER_CREATE_RES,

        CL_MAIN_PLAYER_INFO_NFY,
        CL_INFO_END_NFY,

        CL_PLAYER_LEVEL_RES,
        CL_PLAYER_EXPERIENCE_RES,

        CL_FRIEND_INFO_NFY,
        CL_FRIEND_REQUEST_REQ,
        CL_FRIEND_REQUEST_RES,
        CL_FRIEND_RESPONSE_REQ,
        CL_FRIEND_RESPONSE_RES,
        CL_FRIEND_REMOVE_REQ,
        CL_FRIEND_REMOVE_RES,
        CL_FRIEND_ONLINE_NFY,
        CL_FRIEND_OFFLINE_NFY,

        CL_ROOM_LIST_NFY,
        CL_ROOM_START_REQ,
        CL_ROOM_START_RES,
        CL_ROOM_START_NFY,
        CL_ROOM_CREATE_REQ,
        CL_ROOM_CREATE_RES,
        CL_ROOM_JOIN_REQ,
        CL_ROOM_JOIN_RES,
        CL_ROOM_LEAVE_REQ,
        CL_ROOM_LEAVE_RES,
        CL_ROOM_SETTING_REQ,
        CL_ROOM_SETTING_RES,
        CL_ROOM_CREATED_NFY,
        CL_ROOM_DESTROYED_NFY,
        CL_ROOM_UPDATED_NFY,
        CL_ROOM_HEARTBEAT_NFY,

        CL_ROOM_KICK_PLAYER_REQ,
        CL_ROOM_KICK_PLAYER_RES,
        CL_ROOM_KICK_PLAYER_NFY,

        CL_ROOM_TRANSFER_OWNER_REQ,
        CL_ROOM_TRANSFER_OWNER_RES,

        CL_ROOM_CHANGE_SLOT_POS_REQ,
        CL_ROOM_CHANGE_SLOT_POS_RES,
        CL_ROOM_CHANGE_SLOT_POS_NFY,
        CL_ROOM_CHANGE_SLOT_SINGLE_POS_NFY,

        CL_PLAYER_STAGE_NFY,
        CL_PLAYER_LOBBY_LIST_REQ,
        CL_PLAYER_LOBBY_LIST_RES,

        CL_PLAYER_JOINED_NFY,
        CL_PLAYER_LEFT_NFY,

        CL_PLAYER_READY_REQ,
        CL_PLAYER_READY_RES,
        CL_PLAYER_READY_NFY,

        CL_CHAT_NORMAL_REQ,
        CL_CHAT_NORMAL_RES,
        CL_CHAT_NORMAL_NFY,
        CL_CHAT_WHISPER_REQ,
        CL_CHAT_WHISPER_NFY,

        // Client <-> RoomServer - Should have CR prefix.
        CR_WELCOME_NFY,

        CR_JOIN_ROOM_REQ,
        CR_JOIN_ROOM_RES,
        CR_JOIN_ROOM_NFY,

        CR_PLAYER_ENTER_NFY,
        CR_PLAYER_LEAVE_NFY,

        CR_PLAYER_UPDATE_ATTRIBUTES_RES,
        
        CR_PLAYER_MOVE_SYNC_NFY,        // Movement sync message sent from client to server
        CR_PLAYER_POS_NFY,              // Position of player, sent from server to client.

        CR_PLAYER_HIT_NFY,
        CR_PLAYER_DIED_NFY,

        CR_IMMUNITY_NFY,
        CR_SPEED_CHANGE_NFY,

        CR_PLACE_BOMB_REQ,
        CR_PLACE_BOMB_RES,

        CR_BOMB_PLACED_NFY,
        CR_BOMB_EXPLODED_NFY,
        CR_BOMB_EXPLODED_OBJECT_NFY,
        CR_BOMB_KICK_REQ,
        CR_BOMB_POS_NFY,

        CR_HURRY_UP_CELL_NFY,

        CR_POWERUP_ADD_NFY,
        CR_POWERUP_REMOVE_NFY,

        CR_MATCH_END_NFY,

        CR_PLAYER_CHAT_NORMAL_REQ,
        CR_PLAYER_CHAT_NORMAL_NFY,

        // Client <-> any server (common messages) - Should have CX prefix.
        CX_TOKEN_REQ,
        CX_TOKEN_RES,

        CX_DISCONNECTED_NFY,

        CX_PLAYER_OFFLINE_NFY,
        CX_PLAYER_ONLINE_NFY,

#if _SERVER  // From here on there are only server messages, so let's remove'em from client.

        // DB <-> RoomServer - Should have DR prefix.
        DR_STARTUP_INFO_REQ,
        DR_LIST_MAP_RES,
        DR_LIST_CELL_TYPES_RES,
        DR_LIST_POWERUP_RES,

        // DB <-> LobbyServer - Should have DL prefix.
        DL_STARTUP_INFO_REQ,
        DL_LIST_MAP_RES,
        DL_FB_AUTH_REQ,
        DL_FB_AUTH_RES,
        DL_AUTH_PLAYER_REQ,
        DL_AUTH_PLAYER_RES,
        DL_REGISTER_REQ,
        DL_REGISTER_RES,
        DL_PLAYER_ADD_INFO_REQ,
        DL_PLAYER_ADD_INFO_RES,
        DL_PLAYER_CREATE_REQ,
        DL_PLAYER_CREATE_RES,
        DL_PLAYER_LEVEL_REQ,
        DL_PLAYER_LEVEL_RES,
        DL_PLAYER_EXPERIENCE_REQ,
        DL_PLAYER_EXPERIENCE_RES,
        DL_FRIEND_REQUEST_REQ,
        DL_FRIEND_REQUEST_RES,
        DL_FRIEND_RESPONSE_REQ,
        DL_FRIEND_RESPONSE_RES,
        DL_FRIEND_REMOVE_REQ,
        DL_FRIEND_REMOVE_RES,

        // DB <-> any server (common messages) - Should have DX prefix.
        DX_TOKEN_PLAYER_REQ,
        DX_TOKEN_PLAYER_RES,

        // LobbyServer <-> RoomServer - Should have LR prefix.
        LR_WELCOME_NFY,
        LR_SERVER_INFO_NFY,
        LR_USER_COUNT_NFY,
        LR_CREATE_ROOM_REQ,
        LR_CREATE_ROOM_RES,
        LR_ROOM_FINISHED_NFY,
        LR_DUMMY_JOIN_NFY,
#endif

        // Meta - Don't use
        MAX = 0xFFFF,
    }

    public enum MessageError
    {
        NONE,
        FAIL,

        SERVER_OFF,

        PLAYER_INVALID,

        INVALID_NAME,
        ALREADY_IN_USE_NAME,

        ROOM_INVALID,
        CREATE_FAIL,
        JOIN_FAIL,
        JOIN_WRONG_PASSWORD,
        NOT_FOUND,
        FULL,
        NOT_ENOUGH,
        NOT_OWNER,
        KICK_YOURSELF,
        TRANSFER_YOURSELF,
        SLOT_CLOSED,
        SLOT_INVALID,

        NOT_READY,
        ALREADY_PLAYING,
        ALREADY_READY,
        ALREADY_CONNECTED,
        AUTH_FAIL,

        INVALID_STATE,

        REGISTER_FAIL,
        REGISTER_LOGIN_IN_USE,
        REGISTER_EMAIL_IN_USE,

        CANT_SELF,
        INVALID_REQUESTER_ID,
        INVALID_REQUESTED_ID,
        ALREADY_EXISTS_REQUESTER,
        ALREADY_EXISTS_REQUESTED,
        ALREADY_FRIENDS,

        MAX = 0xFFFF
    }

    public enum ChatType
    {
        GENERAL = 0,
        SYSTEM,
        NORMAL,
        WHISPER,

        MAX = 0xFFFF
    }

    public enum PlayerGender
    {
        None,
        Male,
        Female
    }

    public enum PlayerStage
    {
        Creating,
        Lobby,
        Room,
        Playing,
    }

    public enum PlayerState
    {
        NotReady,
        Ready,
        Offline,
    }

    public enum RoomStage
    {
        Waiting,
        Full,
        Playing,
    }

    public enum FriendState
    {
        Online,
        Offline,
        WaitingApproval,
        Requested,
    }
}