using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject cell_prefab;
    public GameObject unit_prefab;

    public List<Unit> units;

    private TcpClient client;
    private Thread receive_thread;

    private bool game_initialized;

    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 30;
    }

    private void Start()
    {
        UnityThread.initUnityThread();
        ConnectToServer();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject.layer == (int)Layers.ClickableCell)
                {
                    var cell = hit.transform.gameObject.GetComponent<Cell>();
                    var command = new CommandDTO()
                    {
                        Pos = cell.pos,
                        Ids = GetSelectedUnitsIds()
                    };
                    SendCommand(command);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (receive_thread != null)
            receive_thread.Abort();
    }

    private void ConnectToServer()
    {
        try
        {
            receive_thread = new Thread(new ThreadStart(ReceiveThread));
            //receive_thread.IsBackground = true;
            receive_thread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("Connection error: " + e.Message);
        }
    }

    private void ReceiveThread()
    {
        try
        {
            while (true)
            {
                client = new TcpClient("127.0.0.1", 7331);
                NetworkStream stream = client.GetStream();
                byte[] data = new byte[64];
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.ASCII.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);

                string message = builder.ToString();

                var pg_info = JsonConvert.DeserializeObject<PlaygroundDTO>(message);

                if (game_initialized == false)
                {
                    game_initialized = true;
                    UnityThread.executeInFixedUpdate(new Action(() => InitGame(pg_info)));
                }
                else
                {
                    UnityThread.executeInFixedUpdate(new Action(() => UpdateUnits(pg_info)));
                }

                stream.Close();
                Thread.Sleep(1000/30);
            }
        }
        catch (SocketException e)
        {
            Debug.Log("Socket exception: " + e.Message);
        }
    }

    private void SendCommand(CommandDTO dto)
    {
        try
        {
            var server = new TcpClient("127.0.0.1", 7332);
            NetworkStream stream = server.GetStream();
            if (stream.CanWrite)
            {
                string message = JsonConvert.SerializeObject(dto) + "<theend>";
               
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(message);
              
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                Debug.Log("Client sent " + message);
            }
            stream.Close();
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void InitGame(PlaygroundDTO dto)
    {
        for (int i = 0; i < dto.Size; i++)
        {
            for (int j = 0; j < dto.Size; j++)
            {
                var cell_go = Instantiate(cell_prefab);
                cell_go.GetComponent<Cell>().pos = new Pos2D() { X = i, Y = j };
                cell_go.transform.position = new Vector3(i, cell_go.transform.position.y, j);
            }
        }

        units = new List<Unit>();

        for (int i = 0; i < dto.UnitInfos.Length; i++)
        {
            var u_go = Instantiate(unit_prefab);
            var u = u_go.GetComponent<Unit>();
            u.id = i;
            u_go.transform.position = new Vector3(dto.UnitInfos[i].ParentPos.X,
                u_go.transform.position.y, dto.UnitInfos[i].ParentPos.Y);
            u.last_pos = u_go.transform.position;
            u.next_pos = u_go.transform.position;
            units.Add(u);
        }

        var cam_pos = Camera.main.transform.position;
        Camera.main.transform.position = new Vector3(dto.Size, cam_pos.y, dto.Size / 2);
    }

    private void UpdateUnits(PlaygroundDTO dto)
    {
        foreach (var info in dto.UnitInfos)
        {
            units.Find(u => u.id == info.Id).UpdateState(info);
        }
    }

    private int[] GetSelectedUnitsIds()
    {
        var ids = new List<int>();
        foreach(var u in units)
        {
            if (u.selected)
                ids.Add(u.id);
        }
        return ids.ToArray();
    }
}

public enum Layers
{
    ClickableCell = 8, SelectableUnit
}
