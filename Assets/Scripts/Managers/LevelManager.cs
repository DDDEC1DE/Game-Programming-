using UnityEngine;

public class LevelManager : MonoBehaviour 
{
    public static LevelManager Instance;

    void Awake()
    {
        //This is similar to a singleton in that it only allows one instance to exist and there is instant global 
        //access to the LevelManager using the static Instance member.
        //
        //This will set the instance to this object if it is the first time it is created.  Otherwise it will delete 
        //itself.
        if (Instance == null)
        {
            //This tells unity not to delete the object when you load another scene
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

	void Start () 
    {
 
	}
	
	void Update () 
    {
       
    }

    public Player GetPlayer()
    {
        return m_CurrentPlayer;
    }

    public void RegisterPlayer(Player player)
    {
        m_CurrentPlayer = player;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(5, 5, 3000, 40), "Camera Intro");
    }

    Player m_CurrentPlayer;
}

