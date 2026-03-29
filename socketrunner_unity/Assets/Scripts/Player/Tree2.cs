using UnityEngine;

public class Tree2 : MonoBehaviour
{
	public void setActiveTree2(bool flag)
	{
		if (flag == true)
		{
			this.gameObject.SetActive(true);
		}
		else
		{
			this.gameObject.SetActive(false);
		}
	}

}