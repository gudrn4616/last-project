

public enum eCardType
{
    active,
    weapon,
    equip,
    debuff
}


public enum eCharacterType
{
    playable,
    non_playable,
    npc
}

public enum eRoleType
{
    none,
    target,
    bodyguard,
    hitman,
    psychopass,
}
public enum eCharacterState
{
    NONE = 0,
    BBANG_SHOOTER = 1, // 빵야 시전자
    BBANG_TARGET = 2, // 빵야 대상 (쉴드 사용가능 상태)
    DEATH_MATCH = 3, // 현피 중 자신의 턴이 아닐 때
    DEATH_MATCH_TURN = 4, // 현피 중 자신의 턴
    FLEA_MARKET_TURN = 5, // 플리마켓 자신의 턴
    FLEA_MARKET_WAIT = 6, // 플리마켓 턴 대기 상태
    GUERRILLA_SHOOTER = 7, // 게릴라 시전자
    GUERRILLA_TARGET = 8, // 게릴라 대상
    BIG_BBANG_SHOOTER = 9, // 난사 시전자
    BIG_BBANG_TARGET = 10, // 난사 대상
    ABSORBING = 11, // 흡수 중
    ABSORB_TARGET = 12, // 흡수 대상
    HALLUCINATING = 13, // 신기루 중
    HALLUCINATION_TARGET = 14, // 신기루 대상
    CONTAINED = 15, // 감금 중
}