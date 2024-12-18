systemctl --user stop gitversion
dotnet publish -o /var/www/gitversion/api/
systemctl --user start gitversion
systemctl --user status gitversion
