async function updateStatus(applicationId, status) {
    try {
        const response = await fetch("/MemberApplications/UpdateApplicationLog", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                applicationId: applicationId,
                status: status
            })
        });

        const result = await response.json();

        if (response.ok) {
            console.log(result.status);
        } else {
            console.log(result.msg);
        }
    } catch (error) {
        console.error(error);
    }
}