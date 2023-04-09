# Albumica

- Physical file provider honoring auth and cache headers
- Check IsAdmin in client-app and admin controllers (policy?)
- Persons CRUD
- Tags CRUD
- Categorization UI
- Basket save as list for streaming zip
- Support for streaming zip download
- Explore postgres backup

## OpenAPI

```
cd src/client-app
npx openapi-typescript-codegen --useOptions --input http://localhost:5080/swagger/v1/swagger.json --output ./src/api
```

## Dependencies
```
sudo apt update && sudo apt-get install -y ffmpeg
```

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

`ffprobe -v quiet -show_format -print_format json -i VID_20210831_104209.mp4`

scales to one side, fist -t seconds, -r frame rate
`ffmpeg -i VID_20210828_193034.mp4 -t 4 -r 8 -loop 0 kita.webp -y`

use this
`ffmpeg -r 16 -ss 0 -i VID_20210828_193034.mp4 -loop 0 -lavfi "[0:v]scale=ih*16/9:-1,boxblur=luma_radius=min(h\,w)/20:luma_power=1:chroma_radius=min(cw\,ch)/20:chroma_power=1[bg];[bg][0:v]overlay=(W-w)/2:(H-h)/2,setpts=0.3*PTS,scale=320:-1,crop=h=iw*9/16" -vb 800K -t 00:00:05 preview.webp -y`
