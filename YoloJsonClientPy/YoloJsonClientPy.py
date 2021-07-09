
import socket
import json
from io import StringIO

targetIp = "192.168.179.9"
targetPort = 8070
buffer = 4096

print("connect ", targetIp)

client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

client.connect((targetIp,targetPort))
client.send(b"testmsg\r\n")

for i in range(100):
    jsonText = client.recv(buffer).decode()
    if len(jsonText) < 50: continue
    try:
        comPos = jsonText.find(",")
        braPos = jsonText.find("{")
        #カッコ開きよりもコンマのほうが先にある場合、コンマを" "に置き換える
        if comPos < braPos:
            jsonText = jsonText.replace(',',' ', 1)
        io = StringIO(jsonText)
        yoloJson = json.load(io)
        print(i,")\t",yoloJson)
    except:
        print("<exception>", jsonText,"</exception>")

