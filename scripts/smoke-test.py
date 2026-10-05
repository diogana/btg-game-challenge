import json
import os
import urllib.error
import urllib.request
import uuid

base = os.environ.get("BFF_URL", "http://localhost:5000").rstrip("/")

def call(path, method="GET", body=None, token=None, expected=200):
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = "Bearer " + token
    request = urllib.request.Request(base + "/api/v1/" + path,
        data=json.dumps(body).encode() if body is not None else None,
        headers=headers, method=method)
    try:
        with urllib.request.urlopen(request, timeout=30) as response:
            status, raw = response.status, response.read()
    except urllib.error.HTTPError as error:
        status, raw = error.code, error.read()
    if status != expected:
        raise RuntimeError(f"{method} {path}: expected {expected}, received {status}")
    return json.loads(raw) if raw else None

if __name__ == "__main__":
    suffix = str(uuid.uuid4())
    token = call("auth/token", "POST", {"clientId": "smoke", "clientSecret": "smoke-secret"})["accessToken"]
    assert call("auth/validate", token=token)["valid"]
    call("dashboard", expected=401)
    friend = call("friends", "POST", {"name": "Smoke " + suffix, "email": None}, token, 201)
    game = call("games", "POST", {"title": "Smoke " + suffix, "platforms": ["PS5"]}, token, 201)
    loan = call("loans", "POST", {"gameId": game["id"], "friendId": friend["id"]}, token, 201)
    current = call("library/" + game["id"], token=token)["game"]
    assert current["status"] == "Borrowed" and current["friendId"] == friend["id"]
    call("loans", "POST", {"gameId": game["id"], "friendId": friend["id"]}, token, 409)
    call("friends/" + friend["id"], "DELETE", token=token, expected=409)
    call("loans/" + loan["id"] + "/return", "POST", {}, token)
    call("loans/" + loan["id"] + "/return", "POST", {}, token, 409)
    assert call("library/" + game["id"], token=token)["game"]["status"] == "Available"
    call("dashboard", token=token)
    call("friends/" + friend["id"] + "/summary", token=token)
    call("games/" + game["id"], "DELETE", token=token, expected=204)
    call("friends/" + friend["id"], "DELETE", token=token, expected=204)
    assert call("loans/" + loan["id"], token=token)["friendName"] == friend["name"]
    print("Smoke test passed: authentication, CRUD, loan, conflict, return, BFF and history.")
