# Abyss on Docker
Docker is a powerful containerization and orchestration platform that is heavily recommended to use to run Abyss on.  
Abyss has a bundled Dockerfile that downloads and installs all the requirements it needs.  
  
There are Bash scripts that I have written that automate most of this stuff, which are available at [abyssal512/abyss-scripts](http://github.com/abyssal512/abyss-scripts).

### Running Abyss on Docker
0) Install Docker.
1) Create a Docker volume for Abyss to store it's configuration and data in:
```bash
> docker volume create abyss-data
abyss-data
```
2) Inspect that Docker volume and note the mountpoint:
```bash
> docker volume inspect abyss-data
[
    {
        "CreatedAt": "2019-08-05T02:01:25-04:00",
        "Driver": "local",
        "Labels": {},
        "Mountpoint": "/var/lib/docker/volumes/abyss-data/_data",
        "Name": "abyss-data",
        "Options": {},
        "Scope": "local"
    }
]
```
3) Copy your `abyss.json` configuration file into the mountpoint:
```bash
> sudo cp abyss.json /var/lib/docker/volumes/abyss-data/_data
```
4) Build the Docker container image:
```bash
> docker build http://github.com/abyssal512/abyss.git
Successfully built cc70ffd710a2
```
5) Run the Docker container image, mounting the volume at /data: (Replace cc70ffd710a2 with your image ID as printed above)
```bash
> docker run -d --name abyssconsole --mount source=abyss-data,target=/data cc70ffd710a2
```
6) Abyss should now be running! If you have any issues, feel free to join [the support server.](https://discord.gg/RsRps9M)
