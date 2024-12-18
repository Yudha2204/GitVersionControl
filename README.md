# Git Version Control
.Net Project untuk support cek versi Repository git yang digunakan pada tiap sub domain soficloud
## Authors
[Yudha](https://github.com/yudha2204)

## Requirement
Butuh install 
[.Net 5.0.0](https://dotnet.microsoft.com/en-us/download/dotnet/5.0)

## Penggunaan

### Membuat versi di git
Pilih branch yang ingin dibuat versi

```bash
git tag -a {version} -m {pesan}
git push origin {version}
```

### Mendapatkan versi (Running lokal)
| Parameter   | Type data            | Keterangan  | Contoh |
| ---- |:------:| -----------------------:|------------:|
| path | string(Required) | Lokasi repository lokal |D:\yudha\repo|
[http://localhost:21855/api/current-version?path={path}]()

### Mengupdate versi lokal sama dengan remote (Running lokal)
| Parameter   | Type data            | Keterangan  | Contoh |
| ---- |:------:| -----------------------:|------------:|
| path | string (Required) | Lokasi repository lokal |D:\yudha\repo|
[http://localhost:21855/api/fetch-version?path={path}]()

### Mengubah versi lokal (Running lokal)
| Parameter   | Type data            | Keterangan  | Contoh |
| ---- |:------:| -----------------------:|:------------:|
| path | string (Required)  | Lokasi repository lokal |D:\yudha\repo|
| version | string (Required) | versi yang ingin diganti | v1.0.3 |
| branch | string (Default = master) | version dari branch mana |master|
[http://localhost:21855/api/fetch-version?path={path}]()

### Mendapatkan semua versi serta pesan tiap versi (Running lokal)
| Parameter   | Type data            | Keterangan  | Contoh |
| ---- |:------:| -----------------------:|------------:|
| path | string (Required) | Lokasi repository lokal |D:\yudha\repo|
[http://localhost:21855/api/versions?path={path}]()