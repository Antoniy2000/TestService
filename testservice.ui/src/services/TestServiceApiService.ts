import Config from "../config";

class TestServiceApiService {
    
    public static async deleteItem(id:string) {
        await fetch(`${Config.ApiUrl}/Converter/DeleteItem?id=${id}`, {
            method: 'delete'
        });
    } 

    public static async downloadConvertedResult(id:string) {
        const res = await fetch(`${Config.ApiUrl}/Converter/GetConvertedData?id=${id}`);
        const header = res.headers.get('Content-Disposition');
        console.log(header)
        const parts = header!.split(';')[2].split('=')[1];
        const filename = decodeURIComponent(parts.slice(parts.indexOf("''") + 2))
        console.log(filename)
        const blob = await res.blob();
        const link = document.createElement('a');
        link.download = filename
        link.href = URL.createObjectURL(blob);
        link.click();
    }
}

export default TestServiceApiService;