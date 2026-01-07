using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Character/Character Data")]
public class CharacterData : ScriptableObject
{
    public string CharacterID;
    public GameObject CharacterPrefab;
    public Sprite Icon;
}
