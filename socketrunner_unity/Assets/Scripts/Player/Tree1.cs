using UnityEngine;

public class Tree1 : MonoBehaviour
{
	public void setActiveTree1(bool flag)
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