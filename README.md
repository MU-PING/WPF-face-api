# WPF-face-api
## 程式簡介
### 使用說明
* 使用 WPF 搭配 Azure face API 【[API document](https://dev.cognitive.azure.cn/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f30395236)】實作人臉識別，步驟：
1. 創建 Person Group【PersonGroup - Create】

3. 創建 Person【PersonGroup Person - Create  】

5. 在 Person 中加入 Face【PersonGroup Person - Add Face】

7. 訓練模型【PersonGroup - Train】

9. 驗證模型【Face - Detect + Identify & PersonGroup Person - Get】
    * Face - Detect：取得 image ID
    * Face - Identify：用 image ID 去取得信心值與對應的 person ID
    * PersonGroup Person - Get：用 person ID 去取得 person name
    
### 範例圖
![20220102_074141](https://user-images.githubusercontent.com/93152909/150055197-da8ebcf6-782d-449c-9967-4600a020b28e.gif)

## Windows Presentation Foundation (WPF)
* 由 Microsoft 開發的一款可建立桌面用戶端應用程式的 UI 架構
* WPF 使用可延伸應用程式標記語言 (XAML) 來提供應用程式設計的宣告式模型
* 透過 GUI 來設計應用程式的GUI介面
### 建立 Windows Presentation Foundation (WPF) 專案
* Visual Studio中 新建專案 -> WPF應用程式

![建立WFP](https://user-images.githubusercontent.com/93152909/148510458-d9e0d180-48ee-4ff0-a719-4c026f014fa0.JPG)


