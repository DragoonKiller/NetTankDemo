onStart = function()
    this.gameObject:SetActive(false)
    local DNS = System.Net.Dns
    ipInput.text = DNS.GetHostEntry(DNS.GetHostName()).AddressList[0]:ToString()
    portInput.text = 0
end
