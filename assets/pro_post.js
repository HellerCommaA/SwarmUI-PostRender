
postParamBuildSteps.push(() => {
    console.log('AEH: post param javascript thign');
    let proPostGroup = document.getElementById('input_group_content_filmgrain');
    console.log(JSON.stringify(currentBackendFeatureSet));
    if (proPostGroup && !currentBackendFeatureSet.includes('feature_flag_post_render')) {
        proPostGroup.append(createDiv(`pro_post_install_button`, 'keep_group_visible', `<button class="basic-button" onclick="installFeatureById('feature_flag_post_render', 'pro_post_install_button')">Install ProPost Nodes</button>`));
    }
});
