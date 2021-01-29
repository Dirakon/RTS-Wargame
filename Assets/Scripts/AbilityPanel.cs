using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityPanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        mine = GetComponent<Image>();
       
    }
    Image mine;
    [SerializeField]
    AbilityIcon abilityIconPrefab;
    private void Awake()
    {
        singleton = this;
        abilities = new AbilityIcon[0];
    }
    [SerializeField]
    int sizeOfAbility=35;
    [SerializeField]
    int spaceBetween=10;
    public static AbilityPanel singleton;
    AbilityIcon[] abilities;
    // Update is called once per frame
    [SerializeField]
    Sprite[] abilitySprites = default;
    public Vector2Int previosSize;
    public void ClearAbilities()
    {
        foreach (var p in abilities)
        {
            p.SetInactive();
        }
    }
    public void SetAbility(Ability ass)
    {
        abilities[ass.id].AttachAbility(ass);
    }
    public Canvas canvas;
    void OnSizeChange(Vector2Int size, Vector2 pixelSize)
    {
        AbilityIcon[] newAbilities = new AbilityIcon[size.x * size.y];
        for (int y = size.y-1, i = 0; y >= 0; --y)
        {
            float localYCoord = y * (sizeOfAbility + spaceBetween) - pixelSize.y / 2 + (spaceBetween + sizeOfAbility) * 0.5f;
            for (int x = 0; x < size.x; ++x, ++i)
            {
                float localXCoord = x * (sizeOfAbility + spaceBetween) - pixelSize.x / 2 + (spaceBetween + sizeOfAbility) * 0.5f; ;
                var inst = Instantiate(abilityIconPrefab,transform);
                inst.Instantiate();
                inst.transform.localScale = new Vector3(1 / inst.image.rectTransform.rect.width, 1 / inst.image.rectTransform.rect.height, 1) * sizeOfAbility;
                //  inst.transform.position = (Vector2)transform.position + new Vector2(localXCoord, localYCoord);
                inst.transform.localPosition = new Vector2(localXCoord, localYCoord);
                if (i < abilities.Length && abilities[i].enabled)
                {
                    inst.AttachAbility(abilities[i].attachedAbility);
                }else
                {
                    inst.SetInactive();
                }
                newAbilities[i] = inst;
            }
        }
        foreach(var p in abilities)
        {
            Destroy(p.gameObject);
        }
        previosSize = size;
        abilities = newAbilities;
    }
    void Update()
    {
        Vector2 pixelSize = new Vector2(mine.rectTransform.rect.width, mine.rectTransform.rect.height);
        Vector2Int size = new Vector2Int((int)(pixelSize.x / (sizeOfAbility + spaceBetween)), (int)(pixelSize.y / (sizeOfAbility + spaceBetween)));
        if (size != previosSize)
        {
            OnSizeChange(size, pixelSize);
        }
    }
    public Sprite GetSprite(Ability ass) { return abilitySprites[(int)ass.abilitySprite]; }
}
public enum AbilitySprite {
    Supersonicbullet,
    Target,
    Explosion,
    MissileSwarm,
    Ground,
    Plane,
    Ship,
    Speed,
    Attack,
    HoldFire,
    Stop,
    LazerMode,
}
