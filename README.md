# Albumica
todo

## Local DeepStack server
```
docker run --rm \
  --name=deepstack \
  --net=gunda \
  -d \
  -e TZ=Europe/Zagreb \
  -e VISION-FACE=True \
  -e MODE=High \
  -e ADMIN-KEY=Secretadminkey \
  -v ~/repos/albumica/src/appdata/deepstack:/datastore \
  deepquestai/deepstack
```

### Misc
```
dotnet outdated ./src/
dotnet ef migrations add -o Data/Migrations --no-build Initial
```

`ffprobe -v quiet -show_format -print_format json -i VID_20210815_124520.mp4`