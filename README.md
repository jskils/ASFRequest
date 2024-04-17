# ASFRequest

---

## 描述

ASFRequest 是为[ArchiSteamFarm](https://github.com/JustArchiNET/ArchiSteamFarm) 开发的一款拓展插件，使其可以利用ASF的一切能力转发所有请求。

使用场景：

- 不熟悉.net开发，通过网络接口的方式拓展任意功能
- 临时需要某个功能但是不想开发插件，不想重启ASF
- 需要批量对机器人统一进行某些操作
- ..........................

---

## 安装方式

1. 从 GitHub Releases 下载插件
2. 将 ASFRequest.dll 解压至 ASF 的 plugins 文件夹
3. 重新启动 ArchiSteamFarm

---

## 接口列表

	swagger接口文档地址：http://{host}:{port}/swagger

| 接口                                    | 类型     | 参数                                               | 备注           |
|---------------------------------------|--------|--------------------------------------------------|--------------|
| `/Api/ASFRequest/GetState/{botNames}` | `POST` | `Url,WithSession,Referer,Headers`                | GET请求返回状态    |
| `/Api/ASFRequest/GetHtml/{botNames}`  | `POST` | `Url,WithSession,Referer,Headers,Xpath`          | GET请求返回Html  |
| `/Api/ASFRequest/GetJson/{botNames}`  | `POST` | `Url,WithSession,Referer,Headers`                | GET请求返回Json  |
| `/Api/ASFRequest/PostHtml/{botNames}` | `POST` | `Url,WithSession,Referer,Headers,Xpath,BodyData` | POST请求返回Html |
| `/Api/ASFRequest/PostHtml/{botNames}` | `POST` | `Url,WithSession,Referer,Headers,BodyData`       | POST请求返回Json |

---

## 使用示例

1. 获取游戏[PUBG: BATTLEGROUNDS](https://store.steampowered.com/app/578080/PUBG_BATTLEGROUNDS/) 的评测情况。

	请求方式：
	```
	curl -X 'POST' \
	  'http://localhost:1242/Api/ASFRequest/GetHtml/asf' \
	  -H 'accept: application/json' \
	  -H 'Authentication: password' \
	  -H 'Content-Type: application/json' \
	  -d '{
	  "Url": "https://store.steampowered.com/app/578080/PUBG_BATTLEGROUNDS/",
	  "WithSession": true,
	  "Referer": "https://store.steampowered.com",
	  "Xpath": "//*[@id='\''userReviews'\'']"
	}'

	```

	返回内容：

	```
	{
	  "Result": {
		"zrtym39598": {
		  "Result": "最近评测：褒贬不一(19,583)- 过去 30 天内的 19,583 篇用户评测中有 69% 为好评。全部评测:褒贬不一(2,342,656)- 此游戏的 2,342,656 篇用户评测中有 58% 为好评。",
		  "Message": "OK",
		  "Success": true
		},
		"zruks05235": {
		  "Result": "最近评测：褒贬不一(19,583)- 过去 30 天内的 19,583 篇用户评测中有 69% 为好评。全部评测:褒贬不一(2,342,656)- 此游戏的 2,342,656 篇用户评测中有 58% 为好评。",
		  "Message": "OK",
		  "Success": true
		}
	  },
	  "Message": "OK",
	  "Success": true
	}
	```

2. 添加游戏 [PUBG: BATTLEGROUNDS](https://store.steampowered.com/app/578080/PUBG_BATTLEGROUNDS/) 到愿望单列表。

	请求方式：
	```
	curl -X 'POST' \
	  'http://localhost:1242/Api/ASFRequest/PostJson/asf' \
	  -H 'accept: application/json' \
	  -H 'Authentication: password' \
	  -H 'Content-Type: application/json' \
	  -d '{
	  "Url": "https://store.steampowered.com/api/addtowishlist",
	  "WithSession": true,
	  "Referer": "https://store.steampowered.com",
	  "BodyData": {
		"appid": "2351560"
	  }
	}'
	```

	返回内容：
	
	```
	{
	  "Result": {
		"zrtym39598": {
		  "Result": {
			"success": true,
			"wishlistCount": 10
		  },
		  "Message": "OK",
		  "Success": true
		},
		"zruks05235": {
		  "Result": {
			"success": true,
			"wishlistCount": 7
		  },
		  "Message": "OK",
		  "Success": true
		}
	  },
	  "Message": "OK",
	  "Success": true
	}
	```
---
