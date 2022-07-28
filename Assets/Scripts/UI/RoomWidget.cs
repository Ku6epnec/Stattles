using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomWidget : MonoBehaviour
{
    [SerializeField] private Button _buttonConnect;
    [SerializeField] private TMP_Text _nameLabel;
    [SerializeField] private Image _backGround;

    private bool _status;

    public Button _buttonConnectRoom => _buttonConnect;

    public void AddRoomInfo(string name, bool status)
    {
        _nameLabel.text = name;
        _status = status;
        if (_status)
        {
            _backGround.color = Color.green;
        }
        else
        {
            _backGround.color = Color.red;
        }
    }
}
